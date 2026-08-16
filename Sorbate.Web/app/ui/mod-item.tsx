import { ModRecord } from "./mod-data";

export default async function ModItem({
  iconUrl,
  displayName,
  author,
  internalName,
  version,
  modLoaderVersion,
  timestamp,
}: ModRecord) {
  return (
    <tr>
      <td>
        <img src={iconUrl} />
      </td>
      <td>{displayName}</td>
      <td>{author}</td>
      <td>{internalName}</td>
      <td>{version}</td>
      <td>{modLoaderVersion}</td>
      <td>{timestamp.toDateString()}</td>
    </tr>
  );
}
