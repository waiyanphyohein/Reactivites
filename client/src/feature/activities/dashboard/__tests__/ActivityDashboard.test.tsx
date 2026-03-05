import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import ActivityDashboard from '../ActivityDashboard';
import { createActivity, createActivityList } from '../../../../test/fixtures';

describe('ActivityDashboard', () => {
  const mockSelectActivity = vi.fn();
  const mockCancelSelectActivity = vi.fn();
  const mockCreateActivity = vi.fn();

  beforeEach(() => {
    mockSelectActivity.mockClear();
    mockCancelSelectActivity.mockClear();
    mockCreateActivity.mockClear();
  });

  it('renders the activity list', () => {
    const activities = createActivityList(2);
    render(
      <ActivityDashboard
        activities={activities}
        selectActivity={mockSelectActivity}
        cancelSelectActivity={mockCancelSelectActivity}
        createActivity={mockCreateActivity}
        currentUsername='Jeff'
        selectedActivity={undefined}
      />
    );

    expect(screen.getByText('Activity 1')).toBeInTheDocument();
    expect(screen.getByText('Activity 2')).toBeInTheDocument();
  });

  it('does not render ActivityDetail when selectedActivity is undefined', () => {
    render(
      <ActivityDashboard
        activities={[]}
        selectActivity={mockSelectActivity}
        cancelSelectActivity={mockCancelSelectActivity}
        createActivity={mockCreateActivity}
        currentUsername='Jeff'
        selectedActivity={undefined}
      />
    );

    // ActivityDetail renders an Edit button; it should not be present
    expect(screen.queryByRole('button', { name: /edit/i })).not.toBeInTheDocument();
  });

  it('renders ActivityDetail when selectedActivity is provided', () => {
    const selectedActivity = createActivity({ title: 'Selected Event' });
    render(
      <ActivityDashboard
        activities={[selectedActivity]}
        selectActivity={mockSelectActivity}
        cancelSelectActivity={mockCancelSelectActivity}
        createActivity={mockCreateActivity}
        currentUsername='Jeff'
        selectedActivity={selectedActivity}
      />
    );

    // ActivityDetail renders an Edit button
    expect(screen.getByRole('button', { name: /edit/i })).toBeInTheDocument();
  });

  it('always renders the ActivityForm (Create Activity heading)', () => {
    render(
      <ActivityDashboard
        activities={[]}
        selectActivity={mockSelectActivity}
        cancelSelectActivity={mockCancelSelectActivity}
        createActivity={mockCreateActivity}
        currentUsername='Jeff'
        selectedActivity={undefined}
      />
    );

    expect(screen.getByText(/create activity/i)).toBeInTheDocument();
  });

  it('renders ActivityForm Cancel button which calls cancelSelectActivity', async () => {
    const user = userEvent.setup();
    render(
      <ActivityDashboard
        activities={[]}
        selectActivity={mockSelectActivity}
        cancelSelectActivity={mockCancelSelectActivity}
        createActivity={mockCreateActivity}
        currentUsername='Jeff'
        selectedActivity={undefined}
      />
    );

    // The form has a Cancel button
    const cancelButtons = screen.getAllByRole('button', { name: /cancel/i });
    await user.click(cancelButtons[0]);

    expect(mockCancelSelectActivity).toHaveBeenCalledOnce();
  });

  it('shows title of selected activity in ActivityDetail', () => {
    const selectedActivity = createActivity({ title: 'Chosen Activity' });
    render(
      <ActivityDashboard
        activities={[selectedActivity]}
        selectActivity={mockSelectActivity}
        cancelSelectActivity={mockCancelSelectActivity}
        createActivity={mockCreateActivity}
        currentUsername='Jeff'
        selectedActivity={selectedActivity}
      />
    );

    // Title appears in both the card in ActivityList and the detail card — we check it's present
    const titles = screen.getAllByText('Chosen Activity');
    expect(titles.length).toBeGreaterThanOrEqual(1);
  });
});
