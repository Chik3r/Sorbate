import Image from "next/image";
import ModList from "@/app/ui/mod-list";
import { ListResponse } from "./ui/mod-data";

export default async function Home({
  searchParams,
}: {
  searchParams: Promise<{ [key: string]: string | string[] | undefined }>;
}) {
  const queryParams = await searchParams;
  let page: number = parseInt(queryParams.page?.toString() || "") || 0;
  if (page < 0) {
    page = 0;
  }

  const limitPerPage: number =
    parseInt(queryParams.perPage?.toString() || "") || 20;
  const name: string = queryParams.name?.toString() || "";
  const author: string = queryParams.author?.toString() || "";
  const version: string = queryParams.version?.toString() || "";

  const response = await fetch(
    "http://127.0.0.1:5000/api/data?" +
      new URLSearchParams({
        limit: limitPerPage,
        page: page,
        name: name,
        author: author,
        version: version,
      }),
  );
  console.log(response);
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
    <div>
      <main>
        <form action="/">
          <input name="page" type="number" defaultValue={page} />
          <select name="perPage" defaultValue={limitPerPage}>
            <option value="10">10</option>
            <option value="20">20</option>
            <option value="50">50</option>
            <option value="100">100</option>
          </select>{" "}
          <br />
          <input
            name="name"
            defaultValue={name}
            placeholder="Internal or Display name"
          />
          <input name="author" defaultValue={author} placeholder="Author" />
          <input
            name="version"
            defaultValue={version}
            placeholder="Mod or tML version"
          />{" "}
          <br />
          <button type="submit">Filter</button>
        </form>

        <ModList records={result.records} />
      </main>
    </div>
  );
}
