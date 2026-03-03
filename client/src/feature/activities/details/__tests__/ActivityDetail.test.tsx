import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import ActivityDetail from '../ActivityDetail';
import { createActivity } from '../../../../test/fixtures';

describe('ActivityDetail', () => {
  const mockCancelSelectActivity = vi.fn();
  const activity = createActivity();

  beforeEach(() => {
    mockCancelSelectActivity.mockClear();
  });

  it('renders the activity title', () => {
    render(<ActivityDetail activity={activity} cancelSelectActivity={mockCancelSelectActivity} />);
    expect(screen.getByText('Test Activity')).toBeInTheDocument();
  });

  it('renders the activity description', () => {
    render(<ActivityDetail activity={activity} cancelSelectActivity={mockCancelSelectActivity} />);
    expect(screen.getByText('A great test activity')).toBeInTheDocument();
  });

  it('renders the formatted date', () => {
    const knownActivity = createActivity({ date: '2024-06-15T00:00:00Z' });
    render(<ActivityDetail activity={knownActivity} cancelSelectActivity={mockCancelSelectActivity} />);

    const dateString = new Date('2024-06-15T00:00:00Z').toLocaleDateString();
    expect(screen.getByText(dateString)).toBeInTheDocument();
  });

  it('renders an Edit button', () => {
    render(<ActivityDetail activity={activity} cancelSelectActivity={mockCancelSelectActivity} />);
    expect(screen.getByRole('button', { name: /edit/i })).toBeInTheDocument();
  });

  it('renders a Cancel button', () => {
    render(<ActivityDetail activity={activity} cancelSelectActivity={mockCancelSelectActivity} />);
    expect(screen.getByRole('button', { name: /cancel/i })).toBeInTheDocument();
  });

  it('calls cancelSelectActivity when Cancel is clicked', async () => {
    const user = userEvent.setup();
    render(<ActivityDetail activity={activity} cancelSelectActivity={mockCancelSelectActivity} />);

    await user.click(screen.getByRole('button', { name: /cancel/i }));

    expect(mockCancelSelectActivity).toHaveBeenCalledOnce();
  });

  it('does not call cancelSelectActivity without a click', () => {
    render(<ActivityDetail activity={activity} cancelSelectActivity={mockCancelSelectActivity} />);
    expect(mockCancelSelectActivity).not.toHaveBeenCalled();
  });

  it('renders an image with the correct alt text', () => {
    render(<ActivityDetail activity={activity} cancelSelectActivity={mockCancelSelectActivity} />);
    expect(screen.getByRole('img', { name: 'Test Activity' })).toBeInTheDocument();
  });

  it('renders image src based on category', () => {
    const activity = createActivity({ category: 'Music' });
    render(<ActivityDetail activity={activity} cancelSelectActivity={mockCancelSelectActivity} />);

    const img = screen.getByRole('img');
    expect(img).toHaveAttribute('src', expect.stringContaining('music'));
  });
});
