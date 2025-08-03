// Type declarations for modules without official types
declare module 'bootstrap' {
  export * from 'bootstrap/dist/js/bootstrap';
}

declare module 'bootstrap/dist/js/bootstrap' {
  export class Modal {
    constructor(element: Element, options?: any);
    show(): void;
    hide(): void;
    toggle(): void;
    static getInstance(element: Element): Modal | null;
  }
  
  export class Tooltip {
    constructor(element: Element, options?: any);
    show(): void;
    hide(): void;
    toggle(): void;
    static getInstance(element: Element): Tooltip | null;
  }
  
  export class Popover {
    constructor(element: Element, options?: any);
    show(): void;
    hide(): void;
    toggle(): void;
    static getInstance(element: Element): Popover | null;
  }
  
  export class Dropdown {
    constructor(element: Element, options?: any);
    show(): void;
    hide(): void;
    toggle(): void;
    static getInstance(element: Element): Dropdown | null;
  }
  
  export class Collapse {
    constructor(element: Element, options?: any);
    show(): void;
    hide(): void;
    toggle(): void;
    static getInstance(element: Element): Collapse | null;
  }
  
  export class Tab {
    constructor(element: Element, options?: any);
    show(): void;
    static getInstance(element: Element): Tab | null;
  }
}
