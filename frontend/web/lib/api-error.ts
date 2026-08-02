export interface ProblemDetails {
    type?: string;
    title?: string;
    status?: number;
    detail?: string;
    errors?: Record<string, string[]>;
}

export class ApiError extends Error {
    constructor(
        public status: number,
        public problem: ProblemDetails
    ) {
        super(problem.title || `HTTP ${status}`);
        this.name = 'ApiError';
    }
}