import ModItem from "@/app/ui/mod-item";
import { ModRecord, ListResponse } from "./mod-data";

export default async function ModList() {
  const response = await fetch("http://127.0.0.1:5000/api/data");
  if (!response.ok) {
    // response.text()
    return "guh goh";
  }

  const result = (await response.json()) as ListResponse;
  result.records.map((x) => (x.timestamp = new Date(x.timestamp)));
  // console.log(result);

  // let r1: ModRecord = {
  //   id: 1,
  //   iconUrl: "https://i1.sndcdn.com/artworks-000635088334-u5vv46-t500x500.jpg",
  //   displayName: "a",
  //   author: "Example Author",
  //   internalName: "ExampleMod",
  //   version: "0.1",
  //   modLoaderVersion: "2022.6.0.1",
  //   timestamp: new Date("2022-06-02 16:00:00"),
  // };

  // let r2: ModRecord = {
  //   id: 2,
  //   iconUrl: "https://i1.sndcdn.com/artworks-000635088334-u5vv46-t500x500.jpg",
  //   displayName: "a2",
  //   author: "Example Author",
  //   internalName: "ExampleMod",
  //   version: "0.1",
  //   modLoaderVersion: "2022.6.0.1",
  //   timestamp: new Date("2022-06-02 16:00:00"),
  // };

  // let result = [r1, r2];

  return (
    <table>
      <tbody>
        {result.records.map((record) => (
          <ModItem key={record.id} {...record} />
        ))}
      </tbody>
    </table>
  );
}
