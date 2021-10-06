import { Role } from "../models/role.model";

export interface DecodedToken {
    exp: number;
    iat: number;
    nbf: number;
    role: Role[];
}