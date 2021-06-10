import { User } from "./user";

export interface Photo {
    id: number;
    url: string;
    isMain: boolean;
    isApproved: boolean;
    username: string;
    user: User;
}
