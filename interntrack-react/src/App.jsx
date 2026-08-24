import { BrowserRouter, Routes, Route, Link } from "react-router-dom";

import HomePage from "./pages/HomePage";
import InternsPage from "./pages/InternsPage";
import TasksPage from "./pages/TasksPage";
import DepartmentsPage from "./pages/DepartmentsPage";

function App() {
    return (
        <BrowserRouter>
            <nav>
                <Link to="/">Ana Sayfa</Link>{" | "}
                <Link to="/interns">Stajyerler</Link>{" | "}
                <Link to="/tasks">Görevler</Link>{" | "}
                <Link to="/departments">Departmanlar</Link>
            </nav>

            <Routes>
                <Route path="/" element={<HomePage />} />
                <Route path="/interns" element={<InternsPage />} />
                <Route path="/tasks" element={<TasksPage />} />
                <Route path="/departments" element={<DepartmentsPage />} />
            </Routes>
        </BrowserRouter>
    );
}

export default App;