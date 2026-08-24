import { useState } from "react";

function TaskList({ tasks, onTaskChanged }) {
    const [message, setMessage] = useState("");
    const [isError, setIsError] = useState(false);

    async function deleteTask(id) {
        setMessage("");
        setIsError(false);

        try {
            const response = await fetch(
                `http://localhost:5053/api/tasks/${id}`,
                {
                    method: "DELETE"
                }
            );

            if (!response.ok) {
                let data = null;

                try {
                    data = await response.json();
                } catch {
                    data = null;
                }

                setIsError(true);
                setMessage(
                    data?.message || "Görev silinemedi."
                );

                return;
            }

            setIsError(false);
            setMessage("Görev başarıyla silindi.");

            onTaskChanged();
        } catch (error) {
            console.error(error);

            setIsError(true);
            setMessage("Sunucuya bağlanılamadı.");
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
            const response = await fetch(
                `http://localhost:5053/api/tasks/${task.id}`,
                {
                    method: "PUT",
                    headers: {
                        "Content-Type": "application/json"
                    },
                    body: JSON.stringify(updatedTask)
                }
            );

            let data = null;

            try {
                data = await response.json();
            } catch {
                data = null;
            }

            if (!response.ok) {
                setIsError(true);
                setMessage(
                    data?.message || "Görev güncellenemedi."
                );

                return;
            }

            setIsError(false);
            setMessage(
                data?.message || "Görev güncellendi."
            );

            onTaskChanged();
        } catch (error) {
            console.error(error);

            setIsError(true);
            setMessage("Sunucuya bağlanılamadı.");
        }
    }

    return (
        <div>
            <h3>Görev Listesi</h3>

            {message && (
                <p
                    style={{
                        marginTop: "10px",
                        fontWeight: "bold"
                    }}
                >
                    {isError ? "❌ " : "✅ "}
                    {message}
                </p>
            )}

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