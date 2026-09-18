# frontend rules

- Use the confirmed stack in [project context](../project.md): Vite + React + TypeScript with Tailwind CSS v4 and `react-router-dom`, no component kit. Adding a component kit or another UI framework is a new project decision, not an implementation detail.
- Implement agreed layout, field behavior, validation and loading/empty/error states.
- Keep frontend validation consistent with API rules; backend remains authoritative.
- Document meaningful accessibility and interaction requirements in the screen design.
- Enable TypeScript strict mode; do not use `any` or non-null assertions to bypass a type error.
- Meet WCAG 2.2 AA: keyboard-operable interactive elements, visible focus indicators, sufficient color contrast and text alternatives for non-text content.
- Read configuration through Vite's `import.meta.env`; never embed secrets or API keys in client-side code — anything shipped to the browser is public.
