import { useState } from "react";
import { api } from "../services/api";
import AlertMessage from "./AlertMessage";
import { getErrorMessage } from "../utils/getErrorMessage";

function TaskList({ tasks, onTaskChanged }) {
    const [message, setMessage] = useState("");
    const [isError, setIsError] = useState(false);

    async function deleteTask(id) {
        setMessage("");
        setIsError(false);

        try {
            await api.delete(`/tasks/${id}`);

            setIsError(false);
            setMessage("Görev başarıyla silindi.");

            onTaskChanged();
        } catch (error) {
            setIsError(true);
            setMessage(getErrorMessage(error));
        }
    }

    async function completeTask(task) {
        setMessage("");
        setIsError(false);

        const updatedTask = {
            title: task.title,
            description: task.description,
            status: "Done",
            internId: task.internId
        };

        try {
            const data = await api.put(
                `/tasks/${task.id}`,
                updatedTask
            );

            setIsError(false);
            setMessage(
                data?.message || "Görev güncellendi."
            );

            onTaskChanged();
        } catch (error) {
            setIsError(true);
            setMessage(getErrorMessage(error));
        }
    }

    return (
        <div>
            <h3>Görev Listesi</h3>

            <AlertMessage
                message={message}
                isError={isError}
            />

            {tasks.length === 0 ? (
                <p>Henüz görev yok.</p>
            ) : (
                tasks.map(task => (
                    <div
                        className="task-card"
                        key={task.id}
                    >
                        <strong>{task.title}</strong>
                        {" - "}
                        {task.status}
                        {" - "}
                        {task.intern?.name}

                        <button
                            onClick={() =>
                                completeTask(task)
                            }
                        >
                            Tamamlandı
                        </button>

                        <button
                            onClick={() =>
                                deleteTask(task.id)
                            }
                        >
                            Sil
                        </button>
                    </div>
                ))
            )}
        </div>
    );
}

export default TaskList;