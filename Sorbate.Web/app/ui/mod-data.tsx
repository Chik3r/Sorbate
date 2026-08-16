export type ListResponse = {
  records: ModRecord[];
  total: number;
};

export type ModRecord = {
  id: number;
  iconUrl?: string;
  displayName?: string;
  author: string;
  internalName: string;
  version: string;
  modLoaderVersion: string;
  timestamp: Date;
};
