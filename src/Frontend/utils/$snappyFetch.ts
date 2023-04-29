import { $fetch, FetchOptions, FetchError } from 'ofetch';

interface ResponseMap {
  blob: Blob;
  text: string;
  arrayBuffer: ArrayBuffer;
}
type ResponseType = keyof ResponseMap | 'json';

type SnappyFetchOptions<R extends ResponseType> = FetchOptions<R>; // TODO: Add additional options?

export async function $snappyFetch<TModel, TResponse extends ResponseType = 'json'>(
  path: RequestInfo,
  {
    ...options
  }: SnappyFetchOptions<TResponse> = {}
) {
  const config = useRuntimeConfig();

  let headers: any = {
    ...options?.headers,
    // TODO: Add auth token,
    accept: 'application/json',
  };

  if (options.body instanceof FormData === false) {
    headers = {
      ...headers,
      'Content-Type': 'application/json',
    };
  }

  try {
    return await $fetch<TModel, TResponse>(path, {
      baseURL: config.public.apiBaseUrl,
      ...options,
      headers,
      credentials: 'include',
    });
  } catch(error) {
    if (!(error instanceof FetchError)) {
      throw error;
    }

    // TODO: Handle redirect if not auth'd

    throw error;
  }
}
