# ci-cd rules

- Use GitHub Actions and Docker when runtime setup is authorized.
- Pin selected tool/runtime versions and document required environment configuration.
- Keep secrets outside source and use least necessary workflow permissions.
- Define registry, target, trigger, migration and rollback behavior before enabling CD.
- Record image/version and smoke-test result after an authorized deployment.
- Pin third-party GitHub Actions to a full commit SHA, not a tag or branch; default workflow permissions to read-only and grant write scopes only to the jobs that need them.
- Prefer OIDC federation over long-lived cloud credentials stored as repository secrets.
- Build container images from a minimal, maintained base image, run as a non-root user, and scan images for known vulnerabilities before publishing.
