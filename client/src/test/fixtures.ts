export const createActivity = (overrides: Partial<Activity> = {}): Activity => ({
  id: '11111111-1111-1111-1111-111111111111',
  title: 'Test Activity',
  date: '2024-06-15T12:00:00Z',
  description: 'A great test activity',
  category: 'music',
  city: 'London',
  venue: 'The O2 Arena',
  latitude: 51.5,
  longitude: -0.1,
  ...overrides,
});

export const createActivityList = (count = 3): Activity[] =>
  Array.from({ length: count }, (_, i) =>
    createActivity({
      id: `${String(i + 1).padStart(8, '0')}-0000-0000-0000-000000000000`,
      title: `Activity ${i + 1}`,
      description: `Description ${i + 1}`,
      category: ['music', 'drinks', 'culture'][i % 3],
    })
  );
