# New API Call: $ARGUMENTS

Add a new Axios API call connecting the React frontend to the .NET backend.

## Axios Agent Location

All API calls are centralised in `client/src/lib/` — typically an `agent.ts` or similar file.
Do not call Axios directly in components.

## Pattern

```ts
// client/src/lib/agent.ts

import axios, { AxiosResponse } from 'axios';

axios.defaults.baseURL = 'https://localhost:5001/api';

const responseBody = <T>(response: AxiosResponse<T>) => response.data;

const requests = {
  get: <T>(url: string) => axios.get<T>(url).then(responseBody),
  post: <T>(url: string, body: object) => axios.post<T>(url, body).then(responseBody),
  put: <T>(url: string, body: object) => axios.put<T>(url, body).then(responseBody),
  del: <T>(url: string) => axios.delete<T>(url).then(responseBody),
};

// Group endpoints by resource
const Activities = {
  list: () => requests.get<Activity[]>('/activities'),
  details: (id: string) => requests.get<Activity>(`/activities/${id}`),
  create: (activity: Activity) => requests.post<void>('/activities', activity),
  update: (activity: Activity) => requests.put<void>(`/activities/${activity.id}`, activity),
  delete: (id: string) => requests.del<void>(`/activities/${id}`),
};

const agent = { Activities };
export default agent;
```

## Shared Types

Define TypeScript interfaces that mirror the .NET DTOs in `client/src/lib/types.ts`:

```ts
export interface Activity {
  id: string;
  title: string;
  date: string;       // ISO 8601 string
  description: string;
  category: string;
  city: string;
  venue: string;
}
```

## Steps

1. Add the TypeScript interface to `client/src/lib/types.ts` (matching the .NET response shape)
2. Add the Axios call to `client/src/lib/agent.ts` under the appropriate resource group
3. Call `agent.Resource.method()` from the component `useEffect` or event handler
4. Handle loading and error states in the component

## Rules

- Never hardcode the base URL in components — always use the centralised `agent`
- Use `async/await` with try/catch for error handling in components
- Dates come from the API as ISO strings — parse with `new Date()` for display
- The backend runs on `https://localhost:5001` — CORS is pre-configured for `localhost:3000`
