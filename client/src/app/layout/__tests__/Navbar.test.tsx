import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import Navbar from '../Navbar';

describe('Navbar', () => {
  const defaultProps = {
    activeView: 'activities' as const,
    onChangeView: vi.fn(),
    onCreateActivityClick: vi.fn(),
    onLogout: vi.fn(),
    username: 'Jeff',
    avatarUrl: '/images/jeff-placeholder.svg',
  };

  it('renders the app name Reactivities', () => {
    render(<Navbar {...defaultProps} />);
    expect(screen.getByText('Reactivities')).toBeInTheDocument();
  });

  it('renders the Activities navigation link', () => {
    render(<Navbar {...defaultProps} />);
    expect(screen.getByText('Activities')).toBeInTheDocument();
  });

  it('renders the profile trigger with username', () => {
    render(<Navbar {...defaultProps} />);
    expect(screen.getByText('Jeff')).toBeInTheDocument();
  });

  it('renders the Create Activity button', () => {
    render(<Navbar {...defaultProps} />);
    expect(screen.getByRole('button', { name: /create activity/i })).toBeInTheDocument();
  });

  it('calls onCreateActivityClick when Create Activity is clicked', async () => {
    const user = userEvent.setup();
    const onCreateActivityClick = vi.fn();
    render(<Navbar {...defaultProps} onCreateActivityClick={onCreateActivityClick} />);

    await user.click(screen.getByRole('button', { name: /create activity/i }));

    expect(onCreateActivityClick).toHaveBeenCalledOnce();
  });

  it('opens user menu and calls onChangeView when Profile is clicked', async () => {
    const user = userEvent.setup();
    const onChangeView = vi.fn();
    render(<Navbar {...defaultProps} onChangeView={onChangeView} />);

    await user.click(screen.getByText('Jeff'));
    await user.click(screen.getByRole('menuitem', { name: 'Profile' }));

    expect(onChangeView).toHaveBeenCalledWith('profile');
  });

  it('calls onLogout when Logout is clicked', async () => {
    const user = userEvent.setup();
    const onLogout = vi.fn();
    render(<Navbar {...defaultProps} onLogout={onLogout} />);

    await user.click(screen.getByText('Jeff'));
    await user.click(screen.getByRole('menuitem', { name: 'Logout' }));

    expect(onLogout).toHaveBeenCalledOnce();
  });
});
