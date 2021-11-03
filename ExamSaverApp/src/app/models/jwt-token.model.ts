import { Role } from "./role.model";

export class JWTToken {
    token: string;
    issuedAt: Date;
    expiresAt: Date;
    roles: Role[];
}