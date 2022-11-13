import { createContext, useContext } from "react";
import ActivityStore from "./activityStore";

interface IStore {
    activityStore: ActivityStore
}

export const store: IStore = {
    activityStore: new ActivityStore()
}

export const StoreContext = createContext(store)

/// new hook
export function useStore() {
    return useContext(StoreContext)
}