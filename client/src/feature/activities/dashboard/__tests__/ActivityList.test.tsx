import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import ActivityList from '../ActivityList';
import { createActivity, createActivityList } from '../../../../test/fixtures';

describe('ActivityList', () => {
  const mockSelectActivity = vi.fn();

  beforeEach(() => {
    mockSelectActivity.mockClear();
  });

  it('renders all activities', () => {
    const activities = createActivityList(3);
    render(<ActivityList activities={activities} selectActivity={mockSelectActivity} />);

    expect(screen.getByText('Activity 1')).toBeInTheDocument();
    expect(screen.getByText('Activity 2')).toBeInTheDocument();
    expect(screen.getByText('Activity 3')).toBeInTheDocument();
  });

  it('renders an empty list without errors', () => {
    const { container } = render(
      <ActivityList activities={[]} selectActivity={mockSelectActivity} />
    );
    // No activity cards rendered
    expect(screen.queryByRole('button', { name: /view/i })).not.toBeInTheDocument();
    expect(container).toBeTruthy();
  });

  it('renders the correct number of View buttons', () => {
    const activities = createActivityList(4);
    render(<ActivityList activities={activities} selectActivity={mockSelectActivity} />);

    const viewButtons = screen.getAllByRole('button', { name: /view/i });
    expect(viewButtons).toHaveLength(4);
  });

  it('passes selectActivity to each ActivityCard', async () => {
    const user = userEvent.setup();
    const activity = createActivity();
    render(<ActivityList activities={[activity]} selectActivity={mockSelectActivity} />);

    await user.click(screen.getByRole('button', { name: /view/i }));

    expect(mockSelectActivity).toHaveBeenCalledWith(activity.id);
  });

  it('renders a single activity correctly', () => {
    const activity = createActivity({ title: 'Solo Event', city: 'Paris', venue: 'Louvre' });
    render(<ActivityList activities={[activity]} selectActivity={mockSelectActivity} />);

    expect(screen.getByText('Solo Event')).toBeInTheDocument();
    expect(screen.getByText('Paris / Louvre')).toBeInTheDocument();
  });
});
