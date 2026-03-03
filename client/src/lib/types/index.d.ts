type Activity = {
    id: string;
    title: string;
    date: string;
    description: string;
    category: string;
    city: string;
    venue: string;
    latitude?: number;
    longitude?: number;
    creatorDisplayName?: string;
}

type ProfileEvent = {
    id: string;
    title: string;
    date: string;
    description?: string;
    category?: string;
    city: string;
    venue: string;
    creatorDisplayName?: string;
}

type UserProfile = {
    username: string;
    displayName: string;
    avatarUrl: string;
    pastEvents: ProfileEvent[];
    futureEvents: ProfileEvent[];
}
