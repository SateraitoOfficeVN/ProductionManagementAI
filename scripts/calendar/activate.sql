-- Owner-only explicit activation; psql variables: activation_date and time_zone.
-- Stop writes and verify configured timezone/date before approved cutover.
\set ON_ERROR_STOP on
BEGIN;
SET LOCAL lock_timeout = '5s';
SET LOCAL statement_timeout = '15s';
SELECT set_config('pmai.activation_date', :'activation_date', true);
SELECT set_config('pmai.activation_zone', :'time_zone', true);
DO $$ DECLARE activation_text text := current_setting('pmai.activation_date');
BEGIN
    IF activation_text !~ '^[0-9]{4}-[0-9]{2}-[0-9]{2}$'
        OR activation_text::date NOT BETWEEN DATE '0001-01-01' AND DATE '9999-12-31'
        OR to_char(activation_text::date, 'YYYY-MM-DD') <> activation_text THEN
        RAISE EXCEPTION 'An explicit supported activation date is required.';
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_timezone_names WHERE name = current_setting('pmai.activation_zone')) THEN
        RAISE EXCEPTION 'The explicit plant timezone is not recognized by PostgreSQL.';
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_class WHERE oid = 'public.plant_calendar_state'::regclass
        AND pg_has_role(current_user, relowner, 'USAGE')) THEN
        RAISE EXCEPTION 'Calendar activation requires the database owner.';
    END IF;
END $$;
LOCK TABLE public.plant_calendar_state IN EXCLUSIVE MODE;
DO $$ BEGIN
    IF EXISTS (SELECT 1 FROM public.plant_calendar_state) THEN
        RAISE EXCEPTION 'Calendar already activated; inspect existing state rather than overwrite it.';
    END IF;
END $$;
INSERT INTO public.plant_calendar_state (id, activated_on, time_zone_id, revision)
VALUES (1, :'activation_date'::date, :'time_zone', 1);
INSERT INTO public.plant_calendar_weekly_revisions
    (calendar_id, effective_from, working_weekdays, is_withdrawn, is_current, commit_revision)
VALUES (1, :'activation_date'::date, 31, false, true, 1);
COMMIT;
