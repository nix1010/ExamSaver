export class Page {
    currentPage: number;
    totalCount: number;

    constructor(currentPage: number, totalCount: number) {
        this.currentPage = currentPage;
        this.totalCount = totalCount;
    }
}