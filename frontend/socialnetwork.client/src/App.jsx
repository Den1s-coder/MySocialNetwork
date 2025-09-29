import { useState } from 'react'
import reactLogo from './assets/react.svg'
import viteLogo from '/vite.svg'
import './App.css'
import Home from './pages/Home.jsx'
import Login from './pages/Login.jsx'
import Register from './pages/Register.jsx'
import { BrowserRouter, Routes, Route, Link, Navigate } from 'react-router-dom';

function App() {
  return (
      <>
          <BrowserRouter>
              <nav style={{ display: 'flex', gap: 12, padding: 12 }}>
                  <Link to="/">Головна</Link>
                  <Link to="/login">Увійти</Link>
                  <Link to="/register">Регистрація</Link>
              </nav>
              <Routes>
                  <Route path="/" element={<Home />} />
                  <Route path="/login" element={<Login />} />
                  <Route path="/register" element={<Register />} />
              </Routes>
          </BrowserRouter>
    </>
  )
}

export default App
