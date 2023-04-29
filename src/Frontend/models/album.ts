export interface Album {
  slug: string;
  parentSlug: string | null;

  title: string;
  description: string;

  createdOn: string;
  updatedOn: string;

  lastPhotoDate: string | null;
}
