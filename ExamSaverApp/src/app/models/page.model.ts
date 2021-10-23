export class Page {
    currentPage: number;
    pageSize: number;
    totalCount: number;

    constructor(currentPage: number, pageSize: number, totalCount: number) {
        this.currentPage = currentPage;
        this.pageSize = pageSize;
        this.totalCount = totalCount;
    }
}