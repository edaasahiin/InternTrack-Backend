function TaskList({ tasks, onTaskChanged }) {

    function deleteTask(id) {
        fetch(`http://localhost:5053/api/tasks/${id}`, {
            method: "DELETE"
        })
            .then(() => onTaskChanged());
    }

    function completeTask(task) {
        const updatedTask = {
            title: task.title,
            description: task.description,
            status: "Done",
            internId: task.internId
        };

        fetch(`http://localhost:5053/api/tasks/${task.id}`, {
            method: "PUT",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(updatedTask)
        })
            .then(() => onTaskChanged());
    }

    return (
        <div>
            <h3>Görev Listesi</h3>

            {tasks.length === 0 ? (
                <p>Henüz görev yok.</p>
            ) : (
                tasks.map(task => (
                     <div className="task-card" key={task.id}>
                        <strong>{task.title}</strong>
                        {" - "}
                        {task.status}
                        {" - "}
                        {task.intern?.name}

                        <button onClick={() => completeTask(task)}>
                            Tamamlandı
                        </button>

                        <button onClick={() => deleteTask(task.id)}>
                            Sil
                        </button>
                    </div>
                ))
            )}
        </div>
    );
}

export default TaskList;