import { useEffect, useState } from "react";
import TaskForm from "../components/TaskForm";
import TaskList from "../components/TaskList";

function TasksPage() {
    const [tasks, setTasks] = useState([]);

    function loadTasks() {
        fetch("http://localhost:5053/api/tasks")
            .then(response => response.json())
            .then(data => setTasks(data));
    }

    useEffect(() => {
        loadTasks();
    }, []);

    return (
        <div>
            <h2>Görevler</h2>

            <TaskForm onTaskAdded={loadTasks} />

            <TaskList
                tasks={tasks}
                onTaskChanged={loadTasks}
            />
        </div>
    );
}

export default TasksPage;