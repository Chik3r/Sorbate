import ModItem from "@/app/ui/mod-item";
import { ModRecord } from "./mod-data";

export default async function ModList({ records }: { records: ModRecord[] }) {
  return (
    <table>
      <tbody>
        {records.map((record) => (
          <ModItem key={record.id} {...record} />
        ))}
      </tbody>
    </table>
  );
}
