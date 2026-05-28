import axios from 'axios';

/**
 * Base URL for the backend API.
 * - Use VITE_API_URL in .env to override.
 * - If the page is loaded over HTTPS (e.g. Vite + mkcert), defaults to https://localhost:5001
 *   so Safari does not block the request (mixed content). Run API with: dotnet run --launch-profile https
 * - Otherwise defaults to http://localhost:5000 (dotnet run).
 */
export function getApiBaseUrl(): string {
  if (import.meta.env.VITE_API_URL) return import.meta.env.VITE_API_URL;
  if (typeof window !== 'undefined' && window.location?.protocol === 'https:') {
    return 'https://localhost:5001';
  }
  return 'http://localhost:5000';
}

export async function fetchActivities(): Promise<Activity[]> {
  const response = await axios.get(`${getApiBaseUrl()}/api/activities/`);
  return Array.isArray(response.data) ? response.data : [];
}

export async function fetchUserProfile(username: string): Promise<UserProfile> {
  const response = await axios.get(
    `${getApiBaseUrl()}/api/profiles/${encodeURIComponent(username)}`
  );
  return response.data as UserProfile;
}

export async function createActivity(activity: Activity): Promise<Activity> {
  const response = await axios.post(`${getApiBaseUrl()}/api/activities`, activity);
  return response.data as Activity;
}

