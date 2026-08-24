import { useEffect, useState } from "react";
import TaskForm from "../components/TaskForm";
import TaskList from "../components/TaskList";
import AlertMessage from "../components/AlertMessage";
import LoadingMessage from "../components/LoadingMessage";
import { api } from "../services/api";

function TasksPage() {
    const [tasks, setTasks] = useState([]);
    const [isLoading, setIsLoading] = useState(true);
    const [message, setMessage] = useState("");
    const [isError, setIsError] = useState(false);

    async function loadTasks() {
        setIsLoading(true);
        setMessage("");
        setIsError(false);

        try {
            const data = await api.get("/tasks");
            setTasks(data ?? []);
        } catch (error) {
            setIsError(true);
            setMessage(error.message);
        } finally {
            setIsLoading(false);
        }
    }

    useEffect(() => {
        loadTasks();
    }, []);

    return (
        <div>
            <h2>Görevler</h2>

            <TaskForm onTaskAdded={loadTasks} />

            <AlertMessage
                message={message}
                isError={isError}
            />

            {isLoading ? (
                <LoadingMessage />
            ) : (
                <TaskList
                    tasks={tasks}
                    onTaskChanged={loadTasks}
                />
            )}
        </div>
    );
}

export default TasksPage;