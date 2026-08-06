'use client'

import { useState } from 'react';
import { mediaApi } from './media-api';
import { entriesApi } from './entries-api';
import { ApiError } from './api-error';

type UploadState = 'idle' | 'requesting' | 'uploading' | 'attaching' | 'done' | 'error';

export function useUpload() {
    const [state, setState] = useState<UploadState>('idle');
    const [progress, setProgress] = useState(0);
    const [error, setError] = useState<string | null>(null);

    async function upload(file:File, entryId: string) {
        setState('requesting');
        setProgress(0);
        setError(null);

        try {
            // Step 1: request an upload URL from media-api
            const { mediaId, uploadUrl } = await mediaApi.requestUploadUrl({
                contentType: file.type,
                sizeInBytes: file.size,
                originalFileName: file.name,
            });

            // Step 2: PUT directly to MinIO with progress
            setState('uploading');
            await putWithProgress(uploadUrl, file, setProgress);

            // Step 3: tell entries-api to attach this media
            setState('attaching');
            await entriesApi.entries.attachMedia(entryId, mediaId);

            setState('done');
            return { mediaId };
        } catch (error) {
            setState('error');
            if (error instanceof ApiError) {
                setError(error.problem.detail || error.problem.title || 'Upload failed.');
            } else if (error instanceof Error) {
                setError(error.message);
            } else {
                setError('Unknown error.');
            }
            throw error;
        }
    }

    function reset() {
    setState('idle');
    setProgress(0);
    setError(null);
  }

  return {
    state,
    progress,
    error,
    upload,
    reset,
    uploading: state !== 'idle' && state !== 'done' && state !== 'error',
  };
}

// XHR wrapper — the one place we can't use fetch
function putWithProgress(
  url: string,
  file: File,
  onProgress: (pct: number) => void
): Promise<void> {
  return new Promise((resolve, reject) => {
    const xhr = new XMLHttpRequest();

    xhr.upload.addEventListener('progress', e => {
      if (e.lengthComputable) {
        onProgress(Math.round((e.loaded / e.total) * 100));
      }
    });

    xhr.addEventListener('load', () => {
      if (xhr.status >= 200 && xhr.status < 300) resolve();
      else reject(new Error(`Upload failed with HTTP ${xhr.status}`));
    });

    xhr.addEventListener('error', () => reject(new Error('Network error during upload')));
    xhr.addEventListener('abort', () => reject(new Error('Upload aborted')));

    xhr.open('PUT', url);
    xhr.setRequestHeader('Content-Type', file.type);
    xhr.send(file);
  });
}
