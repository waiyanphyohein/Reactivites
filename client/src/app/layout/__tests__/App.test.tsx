import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import axios from 'axios';
import { getApiBaseUrl } from '../../../lib/api';
import App from '../App';

vi.mock('axios');
const mockedAxios = vi.mocked(axios);

const mockActivities: Activity[] = [
  {
    id: '11111111-1111-1111-1111-111111111111',
    title: 'First Activity',
    date: '2024-06-15T12:00:00Z',
    description: 'First Description',
    category: 'music',
    city: 'London',
    venue: 'The O2',
  },
  {
    id: '22222222-2222-2222-2222-222222222222',
    title: 'Second Activity',
    date: '2024-07-20T10:00:00Z',
    description: 'Second Description',
    category: 'drinks',
    city: 'Berlin',
    venue: 'Bar 25',
  },
];

const mockProfile: UserProfile = {
  username: 'jeff',
  displayName: 'Jeff',
  avatarUrl: '/images/jeff-placeholder.svg',
  pastEvents: [
    {
      id: 'past-1',
      title: 'Past Event',
      date: '2025-01-15T12:00:00Z',
      description: 'Past Description',
      category: 'social',
      city: 'New York',
      venue: 'Central Park',
    },
  ],
  futureEvents: [
    {
      id: 'future-1',
      title: 'Future Event',
      date: '2026-07-20T12:00:00Z',
      description: 'Future Description',
      category: 'music',
      city: 'Boston',
      venue: 'MIT Hall',
    },
  ],
};

describe('App', () => {
  beforeEach(() => {
    vi.resetAllMocks();
    mockedAxios.get = vi.fn().mockImplementation((url: string) => {
      if (url === `${getApiBaseUrl()}/api/activities/`) {
        return Promise.resolve({ data: mockActivities });
      }
      if (url === `${getApiBaseUrl()}/api/profiles/jeff`) {
        return Promise.resolve({ data: mockProfile });
      }
      return Promise.reject(new Error(`Unexpected URL: ${url}`));
    });
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('renders the Navbar', async () => {
    render(<App />);
    await waitFor(() => {
      expect(screen.getByText('Reactivities')).toBeInTheDocument();
    });
  });

  it('renders the Create Activity form', async () => {
    render(<App />);
    await waitFor(() => {
      const matches = screen.getAllByText(/create activity/i);
      expect(matches.length).toBeGreaterThanOrEqual(1);
    });
  });

  it('fetches activities on mount and renders them', async () => {
    render(<App />);
    await waitFor(() => {
      expect(screen.getByText('First Activity')).toBeInTheDocument();
      expect(screen.getByText('Second Activity')).toBeInTheDocument();
    });
  });

  it('calls activity and profile API endpoints on mount', async () => {
    render(<App />);
    await waitFor(() => {
      expect(mockedAxios.get).toHaveBeenCalledWith(`${getApiBaseUrl()}/api/activities/`);
      expect(mockedAxios.get).toHaveBeenCalledWith(`${getApiBaseUrl()}/api/profiles/jeff`);
    });
  });

  it('renders no activity cards when API returns empty list', async () => {
    mockedAxios.get = vi.fn().mockImplementation((url: string) => {
      if (url === `${getApiBaseUrl()}/api/activities/`) {
        return Promise.resolve({ data: [] });
      }
      if (url === `${getApiBaseUrl()}/api/profiles/jeff`) {
        return Promise.resolve({ data: mockProfile });
      }
      return Promise.reject(new Error(`Unexpected URL: ${url}`));
    });

    render(<App />);
    await waitFor(() => {
      expect(screen.queryByRole('button', { name: /view/i })).not.toBeInTheDocument();
    });
  });

  it('selecting an activity shows ActivityDetail', async () => {
    const user = userEvent.setup();
    render(<App />);

    await waitFor(() => {
      expect(screen.getByText('First Activity')).toBeInTheDocument();
    });

    const viewButtons = screen.getAllByRole('button', { name: /view/i });
    await user.click(viewButtons[0]);

    await waitFor(() => {
      expect(screen.getByRole('button', { name: /edit/i })).toBeInTheDocument();
    });
  });

  it('cancelling an activity clears the detail view', async () => {
    const user = userEvent.setup();
    render(<App />);

    await waitFor(() => {
      expect(screen.getByText('First Activity')).toBeInTheDocument();
    });

    const viewButtons = screen.getAllByRole('button', { name: /view/i });
    await user.click(viewButtons[0]);

    await waitFor(() => {
      expect(screen.getByRole('button', { name: /edit/i })).toBeInTheDocument();
    });

    const cancelButtons = screen.getAllByRole('button', { name: /cancel/i });
    await user.click(cancelButtons[0]);

    await waitFor(() => {
      expect(screen.queryByRole('button', { name: /edit/i })).not.toBeInTheDocument();
    });
  });

  it('allows navigating to the profile and seeing future events', async () => {
    const user = userEvent.setup();
    render(<App />);

    await waitFor(() => {
      expect(screen.getByText('Jeff')).toBeInTheDocument();
    });

    await user.click(screen.getByText('Jeff'));
    await user.click(screen.getByRole('menuitem', { name: 'Profile' }));

    await waitFor(() => {
      expect(screen.getByText('@jeff')).toBeInTheDocument();
      expect(screen.getByText('Future Event')).toBeInTheDocument();
    });
  });

  it('logs out from dropdown and returns to login page', async () => {
    const user = userEvent.setup();
    render(<App />);

    await waitFor(() => {
      expect(screen.getByText('Jeff')).toBeInTheDocument();
    });

    await user.click(screen.getByText('Jeff'));
    await user.click(screen.getByRole('menuitem', { name: 'Logout' }));

    await waitFor(() => {
      expect(screen.getByRole('heading', { name: /welcome back/i })).toBeInTheDocument();
    });
  });

  it('handles API errors gracefully and renders without activities', async () => {
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
    mockedAxios.get = vi.fn().mockRejectedValue(new Error('Network Error'));

    render(<App />);

    await waitFor(() => {
      expect(consoleSpy).toHaveBeenCalled();
    });

    const matches = screen.getAllByText(/create activity/i);
    expect(matches.length).toBeGreaterThanOrEqual(1);
    consoleSpy.mockRestore();
  });

  it('creates a new activity from the form and renders it in the list', async () => {
    const user = userEvent.setup();
    render(<App />);

    await waitFor(() => {
      expect(screen.getByText('First Activity')).toBeInTheDocument();
    });

    await user.type(screen.getByLabelText(/title/i), 'Created In Test');
    await user.type(screen.getByLabelText(/date/i), '2026-11-12T18:30');
    await user.type(screen.getByLabelText(/description/i), 'Created via form');
    await user.type(screen.getByLabelText(/category/i), 'Networking');
    await user.type(screen.getByLabelText(/city/i), 'New York');
    await user.type(screen.getByLabelText(/venue/i), 'Innovation Loft');
    await user.type(screen.getByLabelText(/latitude/i), '40.71');
    await user.type(screen.getByLabelText(/longitude/i), '-74.00');
    await user.click(screen.getByRole('button', { name: /submit/i }));

    expect(screen.getByText('Created In Test')).toBeInTheDocument();
  });

  it('uses navbar create activity action to return from profile to activities view', async () => {
    const user = userEvent.setup();
    render(<App />);

    await waitFor(() => {
      expect(screen.getByText('Jeff')).toBeInTheDocument();
    });

    await user.click(screen.getByText('Jeff'));
    await user.click(screen.getByRole('menuitem', { name: 'Profile' }));
    await waitFor(() => {
      expect(screen.getByText('@jeff')).toBeInTheDocument();
    });

    await user.click(screen.getByRole('button', { name: /create activity/i }));

    await waitFor(() => {
      expect(screen.getByText(/discover activities/i)).toBeInTheDocument();
    });
  });
});
