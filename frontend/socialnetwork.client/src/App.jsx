import './App.css'
import Home from './pages/Home.jsx'
import Login from './pages/Login.jsx'
import Register from './pages/Register.jsx'
import { BrowserRouter, Routes, Route, Link, Navigate } from 'react-router-dom';
import NavBar from './components/NavBar.jsx';

function App() {
  return (
      <>
          <BrowserRouter>
              <NavBar />
              <div style={{ paddingTop: 56 }}>
                  <Routes>
                    <Route path="/" element={<Home />} />
                    <Route path="/login" element={<Login />} />
                    <Route path="/register" element={<Register />} />
                  </Routes>
              </div>
          </BrowserRouter>
    </>
  )
}

export default App
