import { useEffect, useState } from "react";

function TaskForm({ onTaskAdded }) {
    const [title, setTitle] = useState("");
    const [description, setDescription] = useState("");
    const [status, setStatus] = useState("ToDo");
    const [internId, setInternId] = useState("");
    const [interns, setInterns] = useState([]);

    const [message, setMessage] = useState("");
    const [isError, setIsError] = useState(false);
    const [isSubmitting, setIsSubmitting] = useState(false);

    useEffect(() => {
        fetch("http://localhost:5053/api/interns")
            .then(response => response.json())
            .then(data => setInterns(data))
            .catch(error => {
                console.error(error);
                setIsError(true);
                setMessage("Stajyerler yüklenemedi.");
            });
    }, []);

    async function handleSubmit(event) {
        event.preventDefault();

        setMessage("");
        setIsError(false);
        setIsSubmitting(true);

        const newTask = {
            title,
            description,
            status,
            internId: Number(internId)
        };

        try {
            const response = await fetch(
                "http://localhost:5053/api/tasks",
                {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json"
                    },
                    body: JSON.stringify(newTask)
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

                if (data?.message) {
                    setMessage(data.message);
                } else if (data?.errors) {
                    const firstError = Object.values(data.errors)[0];

                    if (Array.isArray(firstError)) {
                        setMessage(firstError[0]);
                    } else {
                        setMessage("Girilen görev bilgileri geçersiz.");
                    }
                } else {
                    setMessage("Görev eklenemedi.");
                }

                return;
            }

            setIsError(false);
            setMessage(data?.message || "Görev başarıyla eklendi.");

            setTitle("");
            setDescription("");
            setStatus("ToDo");
            setInternId("");

            onTaskAdded();
        } catch (error) {
            console.error(error);

            setIsError(true);
            setMessage("Sunucuya bağlanılamadı.");
        } finally {
            setIsSubmitting(false);
        }
    }

    return (
        <div>
            <h3>Görev Ekle</h3>

            <form onSubmit={handleSubmit}>
                <input
                    placeholder="Görev Başlığı"
                    value={title}
                    onChange={e => setTitle(e.target.value)}
                    required
                />

                <input
                    placeholder="Açıklama"
                    value={description}
                    onChange={e => setDescription(e.target.value)}
                />

                <select
                    value={status}
                    onChange={e => setStatus(e.target.value)}
                >
                    <option value="ToDo">ToDo</option>
                    <option value="InProgress">In Progress</option>
                    <option value="Done">Done</option>
                </select>

                <select
                    value={internId}
                    onChange={e => setInternId(e.target.value)}
                    required
                >
                    <option value="">Stajyer Seç</option>

                    {interns.map(intern => (
                        <option
                            key={intern.id}
                            value={intern.id}
                        >
                            {intern.name}
                        </option>
                    ))}
                </select>

                <button
                    type="submit"
                    disabled={isSubmitting}
                >
                    {isSubmitting ? "Ekleniyor..." : "Görev Ekle"}
                </button>
            </form>

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
        </div>
    );
}

export default TaskForm;