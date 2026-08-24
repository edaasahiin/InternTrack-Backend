import { useEffect, useState } from "react";
import { api } from "../services/api";
import AlertMessage from "./AlertMessage";
import { getErrorMessage } from "../utils/getErrorMessage";

function InternForm({ onInternAdded }) {
    const [name, setName] = useState("");
    const [email, setEmail] = useState("");
    const [departmentId, setDepartmentId] = useState("");
    const [departments, setDepartments] = useState([]);

    const [message, setMessage] = useState("");
    const [isError, setIsError] = useState(false);
    const [isSubmitting, setIsSubmitting] = useState(false);

    useEffect(() => {
        async function loadDepartments() {
            try {
                const data = await api.get("/departments");
                setDepartments(data ?? []);
            } catch (error) {
                setIsError(true);
                setMessage(getErrorMessage(error));
            }
        }

        loadDepartments();
    }, []);

    async function handleSubmit(event) {
        event.preventDefault();

        setMessage("");
        setIsError(false);
        setIsSubmitting(true);

        const newIntern = {
            name,
            email,
            departmentId: Number(departmentId)
        };

        try {
            const data = await api.post("/interns", newIntern);

            setIsError(false);
            setMessage(
                data?.message || "Stajyer başarıyla eklendi."
            );

            setName("");
            setEmail("");
            setDepartmentId("");

            onInternAdded();
        } catch (error) {
            setIsError(true);
            setMessage(getErrorMessage(error));
        } finally {
            setIsSubmitting(false);
        }
    }

    return (
        <div>
            <h3>Stajyer Ekle</h3>

            <form onSubmit={handleSubmit}>
                <input
                    type="text"
                    placeholder="Ad"
                    value={name}
                    onChange={e => setName(e.target.value)}
                    required
                />

                <input
                    type="email"
                    placeholder="Email"
                    value={email}
                    onChange={e => setEmail(e.target.value)}
                    required
                />

                <select
                    value={departmentId}
                    onChange={e => setDepartmentId(e.target.value)}
                    required
                >
                    <option value="">Departman Seç</option>

                    {departments.map(department => (
                        <option
                            key={department.id}
                            value={department.id}
                        >
                            {department.name}
                        </option>
                    ))}
                </select>

                <button
                    type="submit"
                    disabled={isSubmitting}
                >
                    {isSubmitting
                        ? "Ekleniyor..."
                        : "Stajyer Ekle"}
                </button>
            </form>

            <AlertMessage
                message={message}
                isError={isError}
            />
        </div>
    );
}

export default InternForm;