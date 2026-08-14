import { useState } from 'react'
import heroImg from './assets/hero.png'
import './App.css'

function App() {
  const [count, setCount] = useState(0)

  return (
    <>
      <section id="center">
        <div className="hero">
          <img src={heroImg} className="base" width="170" height="179" alt="" />
        </div>
        <div>
          <h1>Get started</h1>
          <p>
            Edit <code>src/App.tsx</code> and save to test <code>HMR</code>
          </p>
        </div>
        <button
          type="button"
          className="counter"
          onClick={() => setCount((count) => count + 1)}
        >
          Count is {count}
        </button>
      </section>

      <div className="ticks"></div>

      <table>
        <tbody>
          <ModItem />
        </tbody>
      </table>
    </>
  )
}

export default App


function ModItem() {
  return (<tr>
    <td>mod icon</td>
    <td>Display name lonbgggg</td>
    <td>Author</td>
    <td>Internal name</td>
    <td>Mod Version</td>
    <td>tModLoader Version</td>
    <td>Time Updated</td>
  </tr>)
}