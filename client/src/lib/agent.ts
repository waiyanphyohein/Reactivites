import axios, { type AxiosResponse } from 'axios';
import { getApiBaseUrl } from './api';

const responseBody = <T>(response: AxiosResponse<T>) => response.data;
const apiUrl = (path: string) => `${getApiBaseUrl()}/api${path}`;

const requests = {
  get: <T>(url: string) => axios.get<T>(url).then(responseBody),
  post: <T>(url: string, body: unknown) => axios.post<T>(url, body).then(responseBody),
};

export const agent = {
  Activities: {
    list: () => requests.get<unknown[]>(apiUrl('/activities/')),
    create: (activity: Activity) => requests.post<unknown>(apiUrl('/activities'), activity),
  },
  Profiles: {
    details: (username: string) =>
      requests.get<UserProfile>(apiUrl(`/profiles/${encodeURIComponent(username)}`)),
  },
};
