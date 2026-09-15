# frontend rules

- Use Vite and TypeScript; UI framework is not selected yet.
- Implement agreed layout, field behavior, validation and loading/empty/error states.
- Keep frontend validation consistent with API rules; backend remains authoritative.
- Document meaningful accessibility and interaction requirements in the screen design.
- Enable TypeScript strict mode; do not use `any` or non-null assertions to bypass a type error.
- Meet WCAG 2.2 AA: keyboard-operable interactive elements, visible focus indicators, sufficient color contrast and text alternatives for non-text content.
- Read configuration through Vite's `import.meta.env`; never embed secrets or API keys in client-side code — anything shipped to the browser is public.
