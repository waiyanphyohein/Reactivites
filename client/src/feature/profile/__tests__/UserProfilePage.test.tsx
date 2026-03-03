import { describe, expect, it } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import UserProfilePage from '../UserProfilePage';

const mockProfile: UserProfile = {
  username: 'jeff',
  displayName: 'Jeff',
  avatarUrl: '/images/jeff-placeholder.svg',
  futureEvents: [
    {
      id: 'future-1',
      title: 'Future Meetup',
      date: '2026-08-20T19:00:00Z',
      description: 'Upcoming meetup',
      category: 'Networking',
      city: 'Boston',
      venue: 'Innovation Hub',
      creatorDisplayName: 'Jeff',
    },
  ],
  pastEvents: [
    {
      id: 'past-1',
      title: 'Past Meetup',
      date: '2025-01-10T19:00:00Z',
      description: 'Past meetup',
      category: 'Social',
      city: 'Chicago',
      venue: 'Town Hall',
      creatorDisplayName: 'SomeoneElse',
    },
  ],
};

describe('UserProfilePage', () => {
  it('renders profile identity information', () => {
    render(<UserProfilePage profile={mockProfile} />);

    expect(screen.getByText('Jeff')).toBeInTheDocument();
    expect(screen.getByText('@jeff')).toBeInTheDocument();
  });

  it('shows future events by default', () => {
    render(<UserProfilePage profile={mockProfile} />);

    expect(screen.getByText('Future Meetup')).toBeInTheDocument();
    expect(screen.queryByText('Past Meetup')).not.toBeInTheDocument();
  });

  it('switches to past events tab', async () => {
    const user = userEvent.setup();
    render(<UserProfilePage profile={mockProfile} />);

    await user.click(screen.getByRole('tab', { name: /past events/i }));

    expect(screen.getByText('Past Meetup')).toBeInTheDocument();
    expect(screen.queryByText('Future Meetup')).not.toBeInTheDocument();
  });

  it('filters to events created by profile owner', async () => {
    const user = userEvent.setup();
    render(<UserProfilePage profile={mockProfile} />);

    await user.click(screen.getByRole('button', { name: /created by me/i }));
    expect(screen.getByText('Future Meetup')).toBeInTheDocument();

    await user.click(screen.getByRole('tab', { name: /past events/i }));
    expect(screen.queryByText('Past Meetup')).not.toBeInTheDocument();
    expect(screen.getByText(/no events available/i)).toBeInTheDocument();
  });
});
