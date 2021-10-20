import { File } from "./file.model";

export class FileInfo extends File {
    fullPath: string;
    isDirectory: boolean;
}