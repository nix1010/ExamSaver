import { Role } from "./role.model";

export class AuthenticationResponse {
    token: string;
    issuedAt: Date;
    expiresAt: Date;
    roles: Role[];
}