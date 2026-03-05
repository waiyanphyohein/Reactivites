import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import ActivityCard from '../ActivityCard';
import { createActivity } from '../../../../test/fixtures';

describe('ActivityCard', () => {
  const mockSelectActivity = vi.fn();
  const activity = createActivity();

  beforeEach(() => {
    mockSelectActivity.mockClear();
  });

  it('renders the activity title', () => {
    render(<ActivityCard activity={activity} selectActivity={mockSelectActivity} />);
    expect(screen.getByText('Test Activity')).toBeInTheDocument();
  });

  it('renders the activity description', () => {
    render(<ActivityCard activity={activity} selectActivity={mockSelectActivity} />);
    expect(screen.getByText('A great test activity')).toBeInTheDocument();
  });

  it('renders city and venue together', () => {
    render(<ActivityCard activity={activity} selectActivity={mockSelectActivity} />);
    expect(screen.getByText('London / The O2 Arena')).toBeInTheDocument();
  });

  it('renders the category as a chip', () => {
    render(<ActivityCard activity={activity} selectActivity={mockSelectActivity} />);
    expect(screen.getByText('music')).toBeInTheDocument();
  });

  it('renders a View button', () => {
    render(<ActivityCard activity={activity} selectActivity={mockSelectActivity} />);
    expect(screen.getByRole('button', { name: /view/i })).toBeInTheDocument();
  });

  it('calls selectActivity with the activity id when View is clicked', async () => {
    const user = userEvent.setup();
    render(<ActivityCard activity={activity} selectActivity={mockSelectActivity} />);

    await user.click(screen.getByRole('button', { name: /view/i }));

    expect(mockSelectActivity).toHaveBeenCalledOnce();
    expect(mockSelectActivity).toHaveBeenCalledWith(activity.id);
  });

  it('renders the formatted date', () => {
    const knownActivity = createActivity({ date: '2024-06-15T00:00:00Z' });
    render(<ActivityCard activity={knownActivity} selectActivity={mockSelectActivity} />);

    // Just check the date is rendered (formatted as locale date)
    const dateString = new Date('2024-06-15T00:00:00Z').toLocaleDateString();
    expect(screen.getByText(dateString)).toBeInTheDocument();
  });

  it('does not call selectActivity without a click', () => {
    render(<ActivityCard activity={activity} selectActivity={mockSelectActivity} />);
    expect(mockSelectActivity).not.toHaveBeenCalled();
  });
});
