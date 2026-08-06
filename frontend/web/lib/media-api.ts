import { apiRequest } from "./api-client";

const BASE_URL = process.env.NEXT_PUBLIC_MEDIA_API_URL!;

// DTOs

export interface RequestUploadUrlInput {
    contentType: string;
    sizeInBytes: number;
    originalFileName: string;
}

export interface UploadUrlResponse {
    mediaId: string;
    uploadUrl: string;
    expiresAtUtc: string;
}

// Client

export const mediaApi = {
    requestUploadUrl: (input: RequestUploadUrlInput) =>
        apiRequest<UploadUrlResponse>({
            baseUrl: BASE_URL!,
            path: `/media/upload-url`,
            json: input,
        }),
};