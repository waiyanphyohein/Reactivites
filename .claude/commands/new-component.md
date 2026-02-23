# New React Component: $ARGUMENTS

Create a new React component for the Reactivities frontend following project conventions.

## Component Structure

```
client/src/feature/<domain>/
├── <FeatureName>.tsx          # Page-level or feature component
├── <FeatureName>Card.tsx      # Presentational sub-component
└── <FeatureName>Form.tsx      # Form component (if applicable)
```

Shared/reusable components that are not feature-specific:
```
client/src/app/layout/         # Layout-level components (Navbar, App shell)
client/src/app/shared/         # Truly generic shared components
```

## Component Conventions

- **Named exports only** — no default exports for components
- **PascalCase** filenames matching the component name (e.g., `ActivityCard.tsx`)
- **`.tsx` extension** for all React components
- **Props interface** defined immediately above the component:
  ```tsx
  interface Props {
    activity: Activity;
    onSelect: (id: string) => void;
  }

  export function ActivityCard({ activity, onSelect }: Props) { ... }
  ```
- No `React.FC` — use plain function declarations with typed props
- Destructure props in the function signature

## Styling

- Use **Material-UI** (`@mui/material`) components — do not use raw HTML elements when an MUI equivalent exists
- Use `sx` prop for one-off styles; avoid inline `style` objects for layout
- Use MUI theme tokens (`theme.spacing`, `theme.palette`) rather than hardcoded values

## State and Data Fetching

- Local UI state: `useState`
- Side effects / data fetching: `useEffect` with a cleanup function or abort controller
- API calls go through Axios in `client/src/lib/` — do not call `fetch` directly
- Types for API responses live in `client/src/lib/types.ts` (or similar shared types file)

## Steps

1. Create the `.tsx` file in the appropriate `feature/<domain>/` directory
2. Define the `Props` interface
3. Implement the component using MUI components
4. Export the component (named export)
5. Import and use in the parent component or route
6. Add Axios call to `client/src/lib/` if new API data is needed
