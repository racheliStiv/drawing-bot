import { useEffect } from 'react'
import './App.css'

function App() {
useEffect(() => {
  fetch("https://localhost:7270/WeatherForecast") 
    .then(res => res.json())
    .then(data => console.log(data));
}, []);

  return (
    <>
      <div>
       
      </div>
      
    </>
  )
}

export default App
