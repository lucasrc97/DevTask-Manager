import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import api from "../services/api";

export default function Dashboard() {
  const [tasks, setTasks] = useState([]);
  const [title, setTitle] = useState("");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const navigate = useNavigate();

  const loadTasks = async () => {
    try {
      const { data } = await api.get("/api/tasks");
      setTasks(data);
      setError("");
    } catch {
      setError("Erro ao carregar tasks.");
    } finally {
      setLoading(false);
    }
  };

  // useEffect carrega as tasks ao montar o componente
  useEffect(() => {
    loadTasks();
  }, []);

  const handleCreate = async (e) => {
    e.preventDefault();
    if (!title.trim()) return;
    try {
      const { data } = await api.post("/api/tasks", { title });
      setTasks([data, ...tasks]);
      setTitle("");
    } catch {
      setError("Erro ao criar task.");
    }
  };

  const handleToggle = async (task) => {
    try {
      const { data } = await api.put(`/api/tasks/${task.id}`, {
        title: task.title,
        isCompleted: !task.isCompleted,
      });
      setTasks(tasks.map((t) => (t.id === task.id ? data : t)));
    } catch {
      setError("Erro ao atualizar task.");
    }
  };

  const handleDelete = async (id) => {
    try {
      await api.delete(`/api/tasks/${id}`);
      setTasks(tasks.filter((t) => t.id !== id));
    } catch {
      setError("Erro ao deletar task.");
    }
  };

  const handleLogout = () => {
    localStorage.removeItem("token");
    navigate("/login");
  };

  return (
    <div className="dashboard">
      <header>
        <h1>DevTask Manager</h1>
        <button className="logout" onClick={handleLogout}>
          Sair
        </button>
      </header>

      <form className="new-task" onSubmit={handleCreate}>
        <input
          type="text"
          value={title}
          onChange={(e) => setTitle(e.target.value)}
          placeholder="Nova task..."
        />
        <button type="submit">Adicionar</button>
      </form>

      {error && <p className="error">{error}</p>}

      {loading ? (
        <p>Carregando...</p>
      ) : tasks.length === 0 ? (
        <p className="empty">Nenhuma task cadastrada.</p>
      ) : (
        <ul className="task-list">
          {tasks.map((task) => (
            <li key={task.id} className={task.isCompleted ? "done" : ""}>
              <input
                type="checkbox"
                checked={task.isCompleted}
                onChange={() => handleToggle(task)}
              />
              <span>{task.title}</span>
              <button className="delete" onClick={() => handleDelete(task.id)}>
                Excluir
              </button>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
