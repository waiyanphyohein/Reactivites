import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import axios from 'axios';
import { getApiBaseUrl } from '../../../lib/api';
import App from '../App';

// Mock axios so we don't make real HTTP calls in tests
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

describe('App', () => {
  beforeEach(() => {
    vi.resetAllMocks();
    mockedAxios.get = vi.fn().mockResolvedValue({ data: mockActivities });
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('renders the Navbar', async () => {
    render(<App />);
    expect(screen.getByText('Reactivities')).toBeInTheDocument();
  });

  it('renders the Create Activity form', async () => {
    render(<App />);
    await waitFor(() => {
      // The form heading is an h5 element; Navbar button is a <button>
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

  it('calls the correct API endpoint on mount', async () => {
    render(<App />);
    await waitFor(() => {
      expect(mockedAxios.get).toHaveBeenCalledWith(`${getApiBaseUrl()}/api/activities/`);
    });
  });

  it('renders no activity cards when API returns empty list', async () => {
    mockedAxios.get = vi.fn().mockResolvedValue({ data: [] });
    render(<App />);
    await waitFor(() => {
      expect(screen.queryByRole('button', { name: /view/i })).not.toBeInTheDocument();
    });
  });

  it('selecting an activity shows ActivityDetail', async () => {
    const user = userEvent.setup();
    render(<App />);

    // Wait for activities to load
    await waitFor(() => {
      expect(screen.getByText('First Activity')).toBeInTheDocument();
    });

    // Find all View buttons and click the first one
    const viewButtons = screen.getAllByRole('button', { name: /view/i });
    await user.click(viewButtons[0]);

    // ActivityDetail should show - it renders an Edit button
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

    // Select an activity
    const viewButtons = screen.getAllByRole('button', { name: /view/i });
    await user.click(viewButtons[0]);

    // Wait for detail to appear
    await waitFor(() => {
      expect(screen.getByRole('button', { name: /edit/i })).toBeInTheDocument();
    });

    // Cancel from the detail card
    const cancelButtons = screen.getAllByRole('button', { name: /cancel/i });
    // The first Cancel in the detail card (not the form)
    await user.click(cancelButtons[0]);

    // Edit button should disappear
    await waitFor(() => {
      expect(screen.queryByRole('button', { name: /edit/i })).not.toBeInTheDocument();
    });
  });

  it('handles API errors gracefully and renders without activities', async () => {
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
    mockedAxios.get = vi.fn().mockRejectedValue(new Error('Network Error'));

    render(<App />);

    await waitFor(() => {
      expect(consoleSpy).toHaveBeenCalledWith('Error fetching activities:', expect.any(Error));
    });

    // App should still render the Navbar button and form heading despite the error
    const matches = screen.getAllByText(/create activity/i);
    expect(matches.length).toBeGreaterThanOrEqual(1);
    consoleSpy.mockRestore();
  });
});
