<script>
import axios from 'axios';
import { API_ENDPOINTS } from '../config';
import { isAuthenticated } from '../utils/auth';
import Material from './Material.vue';

export default {
  name: 'FavoritePage',
  components: {
    Material
  },
  data() {
    return {
      favorites: [],
      isAuthenticated: false,
      loading: false,
      error: null
    };
  },
  mounted() {
    this.checkAuthentication();

    if (this.isAuthenticated) {
      this.fetchFavorites();
    }
  },
  methods: {
    checkAuthentication() {
      this.isAuthenticated = isAuthenticated();
    },

    async fetchFavorites() {
      this.loading = true;
      this.error = null;

      try {
        const response = await axios.get(API_ENDPOINTS.GET_FAVORITES);

        if (response.data) {
          this.favorites = response.data.map(material => ({
            id: material.id || material.Id || material.materialId || material.MaterialId,
            description: material.description || material.Description,
            course: material.course || material.Course,
            subject: material.subject || material.Subject,
            teacherName:
              material.teacher ||
              material.Teacher ||
              material.teacherName ||
              material.TeacherName,
            semester: material.semester || material.Semester,
            date:
              material.dateAdded ||
              material.createdAt ||
              material.CreatedAt ||
              material.addedDate,
            imagesNameUrl: material.imagesNameUrl || material.ImagesNameUrl || {},
            fileNameUrl: material.fileNameUrl || material.FileNameUrl || {}
          }));
        }
      } catch (error) {
        console.error('Ошибка при загрузке избранного:', error);
        this.error = 'Не удалось загрузить список избранных материалов';
      } finally {
        this.loading = false;
      }
    },

    async removeFavorite(id) {
      try {
        const response = await axios.get(`${API_ENDPOINTS.REMOVE_FAVORITE}/${id}`);

        if (response.status === 200) {
          this.favorites = this.favorites.filter(fav => fav.id !== id);
          alert('Материал удален из избранного');
        }
      } catch (error) {
        console.error('Ошибка при удалении из избранного:', error);
        alert('Не удалось удалить материал из избранного');
      }
    },

    viewMaterialDetails(materialId) {
      this.$router.push(`/materials/${materialId}`);
    },

    goToMaterials() {
      this.$router.push('/materials');
    }
  }
};
</script>

<template>
  <div class="favorite-page grid-background">
    <section class="favorites-hero page-container">
      <div class="section-kicker">ИЗБРАННОЕ</div>

      <h1>
        сохранённые
        <span>материалы</span>
      </h1>

      <p>
        Всё, что ты отметил для быстрого доступа, собрано в одном месте.
      </p>
    </section>

    <section class="page-container favorites-content">
      <div v-if="!isAuthenticated" class="state-card">
        <div class="state-icon">♡</div>

        <div>
          <strong>Нужна авторизация</strong>
          <p>
            Войди в аккаунт через меню профиля, чтобы увидеть избранные материалы.
          </p>
        </div>
      </div>

      <div v-else-if="loading" class="state-card">
        <div class="state-icon">…</div>

        <div>
          <strong>Загружаем избранное</strong>
          <p>Это займёт несколько секунд.</p>
        </div>
      </div>

      <div v-else-if="error" class="state-card state-card--error">
        <div class="state-icon">!</div>

        <div>
          <strong>{{ error }}</strong>
          <p>Попробуй обновить страницу чуть позже.</p>
        </div>
      </div>

      <div v-else-if="favorites.length === 0" class="empty-state">
        <div class="empty-mark">♡</div>

        <div class="section-kicker">ПОКА ПУСТО</div>
        <h2>здесь появится избранное</h2>

        <p>
          Добавляй полезные материалы в избранное с помощью сердечка
          на карточке материала.
        </p>

        <button
          class="browse-button"
          type="button"
          @click="goToMaterials"
        >
          Перейти в библиотеку
          <span>→</span>
        </button>
      </div>

      <div v-else>
        <div class="results-heading">
          <div>
            <div class="section-kicker">СОХРАНЕНО</div>
            <h2>твои материалы</h2>
          </div>

          <span class="results-count">{{ favorites.length }}</span>
        </div>

        <div class="material-grid">
          <Material
            v-for="favorite in favorites"
            :key="favorite.id"
            :material="favorite"
            @view-details="viewMaterialDetails"
          />
        </div>
      </div>
    </section>
  </div>
</template>

<style scoped>
.favorite-page {
  min-height: 100vh;
  padding-bottom: 90px;
  color: var(--color-text, #161616);
}

.favorites-hero {
  padding-top: 62px;
  padding-bottom: 40px;
}

.section-kicker {
  color: var(--color-text-secondary, #64645f);
  font-size: 10px;
  font-weight: 800;
  letter-spacing: .14em;
}

.favorites-hero h1 {
  margin: 10px 0 18px;
  font-size: clamp(56px, 7vw, 102px);
  font-weight: 900;
  line-height: .88;
  letter-spacing: -.065em;
}

.favorites-hero h1 span {
  display: inline-block;
  margin-top: 8px;
  padding: 0 12px 7px;
  background: var(--color-green, #dce8c6);
  font-family: Georgia, 'Times New Roman', serif;
  font-weight: 400;
  transform: rotate(-1deg);
}

.favorites-hero p {
  max-width: 580px;
  margin: 0;
  color: var(--color-text-secondary, #64645f);
  font-size: 17px;
  line-height: 1.55;
}

.favorites-content {
  display: flex;
  flex-direction: column;
  gap: 28px;
}

.results-heading {
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  gap: 20px;
  margin-bottom: 24px;
}

.results-heading h2 {
  margin: 6px 0 0;
  font-family: Georgia, 'Times New Roman', serif;
  font-size: 50px;
  font-weight: 400;
  line-height: .95;
  letter-spacing: -.04em;
}

.results-count {
  min-width: 42px;
  height: 42px;
  display: grid;
  place-items: center;
  border: 1px solid var(--color-border-dark, #202020);
  border-radius: 50%;
  background: var(--color-blue, #cbe8f2);
  font-size: 13px;
  font-weight: 800;
}

.material-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(290px, 1fr));
  gap: 18px;
}

.state-card {
  display: flex;
  align-items: center;
  gap: 18px;
  padding: 26px;
  border: 1px dashed var(--color-border-dark, #202020);
  border-radius: 16px;
  background: rgba(255,255,255,.72);
}

.state-icon {
  width: 50px;
  height: 50px;
  flex: 0 0 50px;
  display: grid;
  place-items: center;
  border-radius: 14px;
  background: var(--color-blue, #cbe8f2);
  font-size: 22px;
  font-weight: 800;
}

.state-card--error .state-icon {
  background: #f4dfdf;
  color: #9b3939;
}

.state-card strong {
  display: block;
  font-size: 15px;
}

.state-card p {
  margin: 5px 0 0;
  color: var(--color-text-secondary, #64645f);
  font-size: 12px;
  line-height: 1.5;
}

.empty-state {
  max-width: 760px;
  margin: 12px auto 0;
  padding: 54px 34px;
  text-align: center;
  border: 1px solid var(--color-border-dark, #202020);
  border-radius: 20px;
  background: rgba(255,255,255,.86);
}

.empty-mark {
  width: 68px;
  height: 68px;
  display: grid;
  place-items: center;
  margin: 0 auto 22px;
  border-radius: 50%;
  background: var(--color-cream, #f4ead4);
  font-size: 32px;
}

.empty-state h2 {
  margin: 7px 0 15px;
  font-family: Georgia, 'Times New Roman', serif;
  font-size: clamp(38px, 5vw, 58px);
  font-weight: 400;
  line-height: .95;
  letter-spacing: -.04em;
}

.empty-state p {
  max-width: 490px;
  margin: 0 auto 24px;
  color: var(--color-text-secondary, #64645f);
  font-size: 14px;
  line-height: 1.55;
}

.browse-button {
  min-height: 44px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 22px;
  padding: 0 18px;
  border: 1px solid var(--color-border-dark, #202020);
  border-radius: 999px;
  background: var(--color-text, #161616);
  color: #fff;
  font-size: 12px;
  font-weight: 800;
}

.browse-button:hover {
  background: #30302d;
}

@media (max-width: 680px) {
  .favorites-hero {
    padding-top: 42px;
  }

  .favorites-hero h1 {
    font-size: clamp(48px, 15vw, 72px);
  }

  .material-grid {
    grid-template-columns: 1fr;
  }

  .empty-state {
    padding: 40px 20px;
  }
}
</style>
