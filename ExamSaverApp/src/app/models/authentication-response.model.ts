import { JWTToken } from "./jwt-token.model";

export class AuthenticationResponse {
    firstName: string;
    lastName: string;
    jwtToken: JWTToken;
}