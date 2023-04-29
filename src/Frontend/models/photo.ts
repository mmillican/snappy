export interface Photo {
  id: string;
  albumSlug: string;
  fileName: string;
  savedFileName: string;
  title: string;
  description: string;
  createdOn: string;
  updatedOn: string;
  captureDate: string | null;
  metadata: { [key: string]: string; } | null;
  tags: string[];

  sizes: PhotoSizes;
}

export interface PhotoSizes {
  thumb: string;
  square: string;
  square400: string;
  small: string;
  medium: string;
  large: string;
  full: string;
}
