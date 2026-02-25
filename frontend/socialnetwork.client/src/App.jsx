import './App.css'
import Home from './pages/Home.jsx'
import Login from './pages/Login.jsx'
import Register from './pages/Register.jsx'
import Profile from './pages/Profile.jsx'
import NewPost from './pages/NewPost.jsx'
import Post from './pages/Post.jsx'
import ChatList from './pages/ChatList.jsx'
import Chat from './pages/Chat.jsx'
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import NavBar from './components/NavBar.jsx';
import FriendshipList from './pages/FriendshipList.jsx';
import Settings from './pages/Settings.jsx';

function App() {
  return (
      <>
          <BrowserRouter>
              <NavBar />
              <div style={{ paddingTop: 56, paddingLeft: 200 }}>
                  <Routes>
                    <Route path="/" element={<Home />} />
                    <Route path="/login" element={<Login />} />
                    <Route path="/register" element={<Register />} />
                    <Route path="/profile" element={<Profile />} />
                    <Route path="/user/:idOrName" element={<Profile />}></Route>
                    <Route path="/post/new" element={<NewPost />} />
                    <Route path="/post/:id" element={<Post />} />
                    <Route path="/chats" element={<ChatList />} />
                    <Route path="/chat/:chatId" element={<Chat />} />
                    <Route path="/friends" element={<FriendshipList />} />
                    <Route path="/settings" element={<Settings />} />
                  </Routes>
              </div>
          </BrowserRouter>
    </>
  )
}

export default App
