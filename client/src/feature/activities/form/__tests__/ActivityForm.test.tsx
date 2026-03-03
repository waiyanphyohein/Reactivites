import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import ActivityForm from '../ActivityForm';

describe('ActivityForm', () => {
  const mockCancelSelectActivity = vi.fn();
  const mockCreateActivity = vi.fn();

  beforeEach(() => {
    mockCancelSelectActivity.mockClear();
    mockCreateActivity.mockClear();
  });

  it('renders the Create Activity heading', () => {
    render(<ActivityForm cancelSelectActivity={mockCancelSelectActivity} currentUsername='Jeff' onCreateActivity={mockCreateActivity} />);
    expect(screen.getByText(/create activity/i)).toBeInTheDocument();
  });

  it('renders the Title input field', () => {
    render(<ActivityForm cancelSelectActivity={mockCancelSelectActivity} currentUsername='Jeff' onCreateActivity={mockCreateActivity} />);
    expect(screen.getByLabelText(/title/i)).toBeInTheDocument();
  });

  it('renders the Description input field', () => {
    render(<ActivityForm cancelSelectActivity={mockCancelSelectActivity} currentUsername='Jeff' onCreateActivity={mockCreateActivity} />);
    expect(screen.getByLabelText(/description/i)).toBeInTheDocument();
  });

  it('renders the Category input field', () => {
    render(<ActivityForm cancelSelectActivity={mockCancelSelectActivity} currentUsername='Jeff' onCreateActivity={mockCreateActivity} />);
    expect(screen.getByLabelText(/category/i)).toBeInTheDocument();
  });

  it('renders the City input field', () => {
    render(<ActivityForm cancelSelectActivity={mockCancelSelectActivity} currentUsername='Jeff' onCreateActivity={mockCreateActivity} />);
    expect(screen.getByLabelText(/city/i)).toBeInTheDocument();
  });

  it('renders the Venue input field', () => {
    render(<ActivityForm cancelSelectActivity={mockCancelSelectActivity} currentUsername='Jeff' onCreateActivity={mockCreateActivity} />);
    expect(screen.getByLabelText(/venue/i)).toBeInTheDocument();
  });

  it('renders the Submit button', () => {
    render(<ActivityForm cancelSelectActivity={mockCancelSelectActivity} currentUsername='Jeff' onCreateActivity={mockCreateActivity} />);
    expect(screen.getByRole('button', { name: /submit/i })).toBeInTheDocument();
  });

  it('renders the Cancel button', () => {
    render(<ActivityForm cancelSelectActivity={mockCancelSelectActivity} currentUsername='Jeff' onCreateActivity={mockCreateActivity} />);
    expect(screen.getByRole('button', { name: /cancel/i })).toBeInTheDocument();
  });

  it('calls cancelSelectActivity when Cancel is clicked', async () => {
    const user = userEvent.setup();
    render(<ActivityForm cancelSelectActivity={mockCancelSelectActivity} currentUsername='Jeff' onCreateActivity={mockCreateActivity} />);

    await user.click(screen.getByRole('button', { name: /cancel/i }));

    expect(mockCancelSelectActivity).toHaveBeenCalledOnce();
  });

  it('does not call cancelSelectActivity without a click', () => {
    render(<ActivityForm cancelSelectActivity={mockCancelSelectActivity} currentUsername='Jeff' onCreateActivity={mockCreateActivity} />);
    expect(mockCancelSelectActivity).not.toHaveBeenCalled();
  });

  it('renders 8 input fields', () => {
    render(<ActivityForm cancelSelectActivity={mockCancelSelectActivity} currentUsername='Jeff' onCreateActivity={mockCreateActivity} />);
    // Title, Date, Description, Category, City, Venue, Latitude, Longitude
    // MUI TextField multiline uses textarea; non-multiline uses input
    const allInputs = screen.getAllByRole('textbox');
    expect(allInputs.length).toBeGreaterThanOrEqual(7);
  });

  it('submits a new activity with creator mapping', async () => {
    const user = userEvent.setup();
    render(<ActivityForm cancelSelectActivity={mockCancelSelectActivity} currentUsername='Jeff' onCreateActivity={mockCreateActivity} />);

    await user.type(screen.getByLabelText(/title/i), 'My Creator Activity');
    await user.type(screen.getByLabelText(/date/i), '2026-10-12T14:30');
    await user.type(screen.getByLabelText(/description/i), 'Owned by Jeff');
    await user.type(screen.getByLabelText(/category/i), 'Networking');
    await user.type(screen.getByLabelText(/city/i), 'Boston');
    await user.type(screen.getByLabelText(/venue/i), 'Downtown Hub');
    await user.type(screen.getByLabelText(/latitude/i), '42.36');
    await user.type(screen.getByLabelText(/longitude/i), '-71.06');

    await user.click(screen.getByRole('button', { name: /submit/i }));

    expect(mockCreateActivity).toHaveBeenCalledOnce();
    expect(mockCreateActivity).toHaveBeenCalledWith(
      expect.objectContaining({
        title: 'My Creator Activity',
        creatorDisplayName: 'Jeff',
      })
    );
  });
});
