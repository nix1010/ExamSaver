export class User {
    email: string;
    password: string;

    constructor(email: string = null, password: string = null) {
        this.email = email;
        this.password = password;
    }
}